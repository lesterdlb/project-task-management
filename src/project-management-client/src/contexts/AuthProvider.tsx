import type {
	LoginRequest,
	LoginResponse,
	RegisterRequest,
	UpdateProfileRequest,
} from '@/types/auth.types';
import type { UserDto } from '@/types/user.types';
import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { AuthContext, type AuthContextType } from './AuthContext';
import * as authService from '@/services/authService';
import { LOCAL_STORAGE_KEYS } from '@/lib/constants';

interface AuthProviderProps {
	children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
	const [user, setUser] = useState<UserDto | null>(null);
	const [token, setToken] = useState<string | null>(null);
	const [isLoading, setIsLoading] = useState(true);

	const clearAuth = useCallback(() => {
		setUser(null);
		setToken(null);
		localStorage.removeItem(LOCAL_STORAGE_KEYS.AUTH_TOKEN);
		localStorage.removeItem(LOCAL_STORAGE_KEYS.CURRENT_USER);
	}, []);

	const login = useCallback(
		async (credentials: LoginRequest) => {
			try {
				const response: LoginResponse = await authService.login(credentials);

				setToken(response.token);
				localStorage.setItem(LOCAL_STORAGE_KEYS.AUTH_TOKEN, response.token);

				const userData = await authService.getCurrentUser();

				setUser(userData);
				localStorage.setItem(LOCAL_STORAGE_KEYS.CURRENT_USER, JSON.stringify(userData));
			} catch (error) {
				clearAuth();
				throw error;
			}
		},
		[clearAuth]
	);

	const register = useCallback(async (data: RegisterRequest) => {
		await authService.register(data);
	}, []);

	const logout = useCallback(() => {
		clearAuth();
	}, [clearAuth]);

	const updateProfile = useCallback(
		async (data: UpdateProfileRequest) => {
			if (!token) throw new Error('Not authenticated');

			const updatedUser = await authService.updateProfile(data);
			setUser(updatedUser);
			localStorage.setItem(LOCAL_STORAGE_KEYS.CURRENT_USER, JSON.stringify(updatedUser));
		},
		[token]
	);

	const refreshUserData = useCallback(async () => {
		try {
			const userData = await authService.getCurrentUser();
			setUser(userData);
			localStorage.setItem(LOCAL_STORAGE_KEYS.CURRENT_USER, JSON.stringify(userData));
		} catch (error) {
			console.error('Failed to refresh user data:', error);
			throw error;
		}
	}, []);

	const refreshUser = useCallback(async () => {
		if (!token) return;

		try {
			await refreshUserData();
		} catch (error) {
			console.error('Failed to refresh user:', error);
			clearAuth();
		}
	}, [token, refreshUserData, clearAuth]);

	useEffect(() => {
		const initializeAuth = async () => {
			const storedToken = localStorage.getItem(LOCAL_STORAGE_KEYS.AUTH_TOKEN);
			const storedUser = localStorage.getItem(LOCAL_STORAGE_KEYS.CURRENT_USER);

			if (storedToken && storedUser) {
				try {
					setToken(storedToken);
					setUser(JSON.parse(storedUser) as UserDto);
				} catch (error) {
					console.error('Failed to initialize auth:', error);
					clearAuth();
				}
			}

			setIsLoading(false);
		};

		initializeAuth();
	}, [clearAuth, refreshUserData]);

	const value: AuthContextType = useMemo(
		() => ({
			user,
			token,
			isAuthenticated: !!token && !!user,
			isLoading,
			login,
			register,
			logout,
			updateProfile,
			refreshUser,
		}),
		[user, token, isLoading, login, register, logout, updateProfile, refreshUser]
	);

	return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
