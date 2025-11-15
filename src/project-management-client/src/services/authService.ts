import apiClient from '@/lib/axios';
import {
	type LoginRequest,
	type LoginResponse,
	type RegisterRequest,
	type UpdateProfileRequest,
	type ConfirmEmailRequest,
	type ResendConfirmationRequest,
} from '@/types/auth.types';
import { type UserDto } from '@/types/user.types';

export const login = async (credentials: LoginRequest): Promise<LoginResponse> => {
	const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
	return response.data;
};

export const register = async (data: RegisterRequest): Promise<void> => {
	await apiClient.post('/auth/register', data);
};

export const confirmEmail = async (data: ConfirmEmailRequest): Promise<void> => {
	await apiClient.post('/auth/confirm-email', data);
};

export const resendConfirmation = async (data: ResendConfirmationRequest): Promise<void> => {
	await apiClient.post('/auth/resend-confirmation', data);
};

export const getCurrentUser = async (): Promise<UserDto> => {
	const response = await apiClient.get<UserDto>('/auth/me');
	return response.data;
};

export const updateProfile = async (data: UpdateProfileRequest): Promise<UserDto> => {
	const response = await apiClient.patch<UserDto>('/auth/profile', data);
	return response.data;
};
