export interface LoginRequest {
	email: string;
	password: string;
}

export interface LoginResponse {
	token: string;
	email: string;
	fullName: string;
}

export interface RegisterRequest {
	userName: string;
	email: string;
	fullName: string;
	password: string;
	confirmPassword: string;
}

export interface ConfirmEmailRequest {
	userId: string;
	token: string;
}

export interface ResendConfirmationRequest {
	email: string;
}

export interface UpdateProfileRequest {
	userName?: string;
	email?: string;
	fullName?: string;
}
