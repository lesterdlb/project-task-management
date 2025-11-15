import { type Link } from './api.types';

export const UserRole = {
	Guest: 'Guest',
	Member: 'Member',
	Admin: 'Admin',
} as const;
export type UserRole = (typeof UserRole)[keyof typeof UserRole];

export interface UserDto {
	id: string;
	userName: string;
	email: string;
	fullName: string;
	emailConfirmed: boolean;
	role: UserRole;
	createdAtUtc: string;
	lastModifiedAtUtc?: string;
	links?: Link[];
}

export interface CreateUserRequest {
	userName: string;
	email: string;
	fullName: string;
	password: string;
	role: UserRole;
}

export interface UpdateUserRequest {
	userName: string;
	email: string;
	fullName: string;
	role: UserRole;
	version: string;
}
