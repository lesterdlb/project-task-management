export interface ApiError {
	type: string;
	title: string;
	status: number;
	errors?: Record<string, string[]>;
	detail?: string;
}

export interface PaginationMetadata {
	currentPage: number;
	pageSize: number;
	totalPages: number;
	totalCount: number;
	hasPrevious: boolean;
	hasNext: boolean;
}

export interface PaginatedResponse<T> {
	items: T[];
	metadata: PaginationMetadata;
}

export interface Link {
	href: string;
	rel: string;
	method: string;
	type: string;
	title?: string;
	deprecated?: boolean;
}

export interface QueryParameters {
	page?: number;
	pageSize?: number;
	sort?: string;
	fields?: string;
	search?: string;
}
