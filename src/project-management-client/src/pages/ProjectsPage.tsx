import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { ProjectsTable } from '@/components/projects/ProjectsTable';
import { Pagination } from '@/components/projects/Pagination';
import { getProjects } from '@/services/projectService';
import { useAuth } from '@/hooks/useAuth';
import { type ProjectDto } from '@/types/project.types';
import { type PaginatedResponse } from '@/types/api.types';
import { type ApiError } from '@/types/api.types';
import { DEFAULT_PAGE_SIZE } from '@/lib/constants';

const ProjectsPage = () => {
	const { isAuthenticated } = useAuth();
	const navigate = useNavigate();
	const [projects, setProjects] = useState<ProjectDto[]>([]);
	const [pagination, setPagination] = useState<PaginatedResponse<ProjectDto>['metadata'] | null>(
		null
	);
	const [currentPage, setCurrentPage] = useState(1);
	const [isLoading, setIsLoading] = useState(true);
	const [apiError, setApiError] = useState<string | null>(null);

	useEffect(() => {
		if (!isAuthenticated) {
			navigate('/login');
			return;
		}
		loadProjects(currentPage);
	}, [currentPage, isAuthenticated, navigate]);

	const loadProjects = async (page: number) => {
		setIsLoading(true);
		setApiError(null);

		try {
			const response = await getProjects({
				page,
				pageSize: DEFAULT_PAGE_SIZE,
			});

			setProjects(response.items);
			setPagination(response.metadata);
		} catch (error) {
			const apiErr = error as ApiError;
			setApiError(apiErr.detail || 'Failed to load projects.');
		} finally {
			setIsLoading(false);
		}
	};

	const handlePageChange = (page: number) => {
		setCurrentPage(page);
	};

	return (
		<div className='container mx-auto py-8'>
			<Card>
				<CardHeader></CardHeader>
				<CardContent className='space-y-4'>
					{apiError && (
						<Alert variant='destructive'>
							<AlertDescription>{apiError}</AlertDescription>
						</Alert>
					)}
					{isLoading ? (
						<div className='text-center py-12 text-muted-foreground'>
							Loading projects...
						</div>
					) : (
						<>
							<ProjectsTable
								projects={projects}
								onEdit={() => {}}
								onDelete={() => {}}
							/>
							{pagination && pagination.totalPages > 1 && (
								<Pagination metadata={pagination} onPageChange={handlePageChange} />
							)}
						</>
					)}
				</CardContent>
			</Card>
		</div>
	);
};

export default ProjectsPage;
