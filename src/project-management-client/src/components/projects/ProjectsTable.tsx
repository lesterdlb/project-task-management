import { Edit, Trash2 } from 'lucide-react';
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { StatusBadge } from './StatusBadge';
import { PriorityBadge } from './PriorityBadge';
import { type ProjectDto } from '@/types/project.types';
import { formatDate } from '@/lib/dateUtils';

interface ProjectsTableProps {
	projects: ProjectDto[];
	onEdit: (project: ProjectDto) => void;
	onDelete: (project: ProjectDto) => void;
}

export const ProjectsTable = ({ projects, onEdit, onDelete }: ProjectsTableProps) => {
	if (projects.length === 0) {
		return (
			<div className='text-center py-12 text-muted-foreground'>
				No projects found. Create your first project to get started.
			</div>
		);
	}

	return (
		<div className='border rounded-lg'>
			<Table>
				<TableHeader>
					<TableRow>
						<TableHead>Name</TableHead>
						<TableHead>Description</TableHead>
						<TableHead>Status</TableHead>
						<TableHead>Priority</TableHead>
						<TableHead>Start Date</TableHead>
						<TableHead>End Date</TableHead>
						<TableHead className='text-right'>Actions</TableHead>
					</TableRow>
				</TableHeader>
				<TableBody>
					{projects.map(project => (
						<TableRow key={project.id}>
							<TableCell className='font-medium'>{project.name}</TableCell>
							<TableCell className='max-w-xs truncate'>
								{project.description}
							</TableCell>
							<TableCell>
								<StatusBadge status={project.status} />
							</TableCell>
							<TableCell>
								<PriorityBadge priority={project.priority} />
							</TableCell>
							<TableCell>{formatDate(project.startDate)}</TableCell>
							<TableCell>{formatDate(project.endDate)}</TableCell>
							<TableCell className='text-right'>
								<div className='flex justify-end gap-2'>
									<Button
										variant='ghost'
										size='sm'
										onClick={() => onEdit(project)}
									>
										<Edit className='h-4 w-4' />
									</Button>
									<Button
										variant='ghost'
										size='sm'
										onClick={() => onDelete(project)}
									>
										<Trash2 className='h-4 w-4 text-destructive' />
									</Button>
								</div>
							</TableCell>
						</TableRow>
					))}
				</TableBody>
			</Table>
		</div>
	);
};
