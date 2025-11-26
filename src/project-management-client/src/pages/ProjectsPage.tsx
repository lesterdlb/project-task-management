import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ProjectsTable } from '@/components/projects/ProjectsTable';
import { Pagination } from '@/components/projects/Pagination';
import { CreateProjectDialog } from '@/components/projects/CreateProjectDialog';
import { getProjects } from '@/services/projectService';
import { useAuth } from '@/hooks/useAuth';
import { type ProjectDto } from '@/types/project.types';
import { type PaginatedResponse } from '@/types/api.types';
import { type ApiError } from '@/types/api.types';
import { DEFAULT_PAGE_SIZE } from '@/lib/constants';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';

const ProjectsPage = () => {
	const { isAuthenticated } = useAuth();
	const navigate = useNavigate();
	const [projects, setProjects] = useState<ProjectDto[]>([]);
	const [pagination, setPagination] = useState<PaginatedResponse<ProjectDto> | null>(null);
	const [currentPage, setCurrentPage] = useState(1);
	const [isLoading, setIsLoading] = useState(true);
	const [apiError, setApiError] = useState<string | null>(null);
	const [createDialogOpen, setCreateDialogOpen] = useState(false);

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
			setPagination(response);
		} catch (error) {
			const apiErr = error as ApiError;
			setApiError(apiErr.detail || 'Failed to load projects.');
		} finally {
			setIsLoading(false);
		}
	};

	const handleCreateSuccess = () => {
		loadProjects(1);
		setCurrentPage(1);
	};

	const handlePageChange = (page: number) => {
		setCurrentPage(page);
	};

	return (
		<div className='container mx-auto py-8'>
			<Card>
				<CardHeader>
					<div className='flex items-center justify-between'>
						<CardTitle className='text-3xl font-bold'>Projects</CardTitle>
						<Button onClick={() => setCreateDialogOpen(true)}>
							<Plus className='h-4 w-4 mr-2' />
							Create Project
						</Button>
					</div>
				</CardHeader>
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
								<Pagination
									page={pagination.page}
									totalPages={pagination.totalPages}
									hasPreviousPage={pagination.hasPreviousPage}
									hasNextPage={pagination.hasNextPage}
									onPageChange={handlePageChange}
								/>
							)}
						</>
					)}
				</CardContent>
			</Card>
			<CreateProjectDialog
				open={createDialogOpen}
				onOpenChange={setCreateDialogOpen}
				onSuccess={handleCreateSuccess}
			/>
		</div>
	);
};

export default ProjectsPage;
