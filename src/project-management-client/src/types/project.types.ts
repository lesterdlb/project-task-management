import { type Link } from './api.types';

export const ProjectStatus = {
	Planned: 'Planned',
	Active: 'Active',
	Completed: 'Completed',
	Archived: 'Archived',
} as const;
export type ProjectStatus = (typeof ProjectStatus)[keyof typeof ProjectStatus];

export const ProjectPriority = {
	Low: 'Low',
	Medium: 'Medium',
	High: 'High',
} as const;
export type ProjectPriority = (typeof ProjectPriority)[keyof typeof ProjectPriority];

export const ProjectRole = {
	Viewer: 'Viewer',
	Contributor: 'Contributor',
	Manager: 'Manager',
} as const;
export type ProjectRole = (typeof ProjectRole)[keyof typeof ProjectRole];

export interface ProjectDto {
	id: string;
	name: string;
	description: string;
	startDate: string;
	endDate: string;
	status: ProjectStatus;
	priority: ProjectPriority;
	ownerId: string;
	ownerName?: string;
	createdAtUtc: string;
	lastModifiedAtUtc?: string;
	links?: Link[];
}

export interface CreateProjectRequest {
	name: string;
	description: string;
	startDate: string;
	endDate?: string;
	status: ProjectStatus;
	priority: ProjectPriority;
	ownerId?: string; // Only for admins
}

export interface UpdateProjectRequest {
	name: string;
	description: string;
	startDate: string;
	endDate: string;
	status: ProjectStatus;
	priority: ProjectPriority;
}

export interface AddProjectMemberRequest {
	userId: string;
	role: ProjectRole;
}

export interface ProjectMemberDto {
	userId: string;
	userName: string;
	role: ProjectRole;
	dateJoined: string;
}
