import { createContext } from 'react';
import {
	type LoginRequest,
	type RegisterRequest,
	type UpdateProfileRequest,
} from '@/types/auth.types';
import { type UserDto } from '@/types/user.types';

export interface AuthContextType {
	user: UserDto | null;
	token: string | null;
	isAuthenticated: boolean;
	isLoading: boolean;
	login: (credentials: LoginRequest) => Promise<void>;
	register: (data: RegisterRequest) => Promise<void>;
	logout: () => void;
	updateProfile: (data: UpdateProfileRequest) => Promise<void>;
	refreshUser: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);
