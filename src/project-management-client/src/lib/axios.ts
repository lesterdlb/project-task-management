import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { type ApiError } from '@/types/api.types';
import { LOCAL_STORAGE_KEYS } from './constants';

const API_BASE_URL = import.meta.env['VITE_API_BASE_URL'] || 'https://localhost:7000/api';

export const apiClient = axios.create({
	baseURL: API_BASE_URL,
	headers: {
		'Content-Type': 'application/json',
	},
	timeout: 30000,
});

apiClient.interceptors.request.use(
	(config: InternalAxiosRequestConfig) => {
		const token = localStorage.getItem(LOCAL_STORAGE_KEYS.AUTH_TOKEN);

		if (token && config.headers) {
			config.headers.Authorization = `Bearer ${token}`;
		}

		return config;
	},
	error => {
		return Promise.reject(error);
	}
);

apiClient.interceptors.response.use(
	response => response,
	(error: AxiosError<ApiError>) => {
		if (error.response) {
			const apiError: ApiError = error.response.data || {
				type: 'about:blank',
				title: 'An error occurred',
				status: error.response.status,
				detail: error.message,
			};

			if (error.response.status === 401) {
				localStorage.removeItem(LOCAL_STORAGE_KEYS.AUTH_TOKEN);
				localStorage.removeItem(LOCAL_STORAGE_KEYS.CURRENT_USER);
				window.location.href = '/login';
			}

			return Promise.reject(apiError);
		} else if (error.request) {
			const networkError: ApiError = {
				type: 'network-error',
				title: 'Network Error',
				status: 0,
				detail: 'Unable to connect to the server. Please check your internet connection.',
			};
			return Promise.reject(networkError);
		} else {
			const unknownError: ApiError = {
				type: 'unknown-error',
				title: 'Unknown Error',
				status: 0,
				detail: error.message || 'An unexpected error occurred',
			};
			return Promise.reject(unknownError);
		}
	}
);

export default apiClient;
