import apiClient from '@/lib/axios';
import {
	type ProjectDto,
	type CreateProjectRequest,
	type UpdateProjectRequest,
} from '@/types/project.types';
import { type PaginatedResponse, type QueryParameters } from '@/types/api.types';

export const getProjects = async (
	params?: QueryParameters
): Promise<PaginatedResponse<ProjectDto>> => {
	const response = await apiClient.get<PaginatedResponse<ProjectDto>>('/projects', { params });
	return response.data;
};

export const getProject = async (id: string): Promise<ProjectDto> => {
	const response = await apiClient.get<ProjectDto>(`/projects/${id}`);
	return response.data;
};

export const createProject = async (data: CreateProjectRequest): Promise<ProjectDto> => {
	const response = await apiClient.post<ProjectDto>('/projects', data);
	return response.data;
};

export const updateProject = async (
	id: string,
	data: UpdateProjectRequest
): Promise<ProjectDto> => {
	const response = await apiClient.put<ProjectDto>(`/projects/${id}`, data);
	return response.data;
};

export const deleteProject = async (id: string): Promise<void> => {
	await apiClient.delete(`/projects/${id}`);
};
