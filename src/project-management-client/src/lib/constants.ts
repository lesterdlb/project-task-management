export const APP_NAME = 'Project Management';

export const ROUTES = {
	HOME: '/',
	LOGIN: '/login',
	REGISTER: '/register',
	PROFILE: '/profile',
	PROJECTS: '/projects',
} as const;

export const LOCAL_STORAGE_KEYS = {
	AUTH_TOKEN: 'authToken',
	CURRENT_USER: 'currentUser',
} as const;

export const DEFAULT_PAGE_SIZE = 10;
export const MAX_PAGE_SIZE = 100;

export const TOAST_DURATION = 5000;
