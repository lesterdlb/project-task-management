import { Badge } from '@/components/ui/badge';
import { type ProjectStatus } from '@/types/project.types';
import { cn } from '@/lib/utils';

interface StatusBadgeProps {
	status: ProjectStatus;
}

const statusStyles: Record<ProjectStatus, string> = {
	Planned: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
	Active: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
	Completed: 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300',
	Archived: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300',
};

export const StatusBadge = ({ status }: StatusBadgeProps) => {
	return <Badge className={cn('capitalize', statusStyles[status])}>{status}</Badge>;
};
