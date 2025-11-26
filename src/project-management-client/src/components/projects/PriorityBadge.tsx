import { Badge } from '@/components/ui/badge';
import { type ProjectPriority } from '@/types/project.types';
import { cn } from '@/lib/utils';

interface PriorityBadgeProps {
	priority: ProjectPriority;
}

const priorityStyles: Record<ProjectPriority, string> = {
	Low: 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300',
	Medium: 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-300',
	High: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
};

export const PriorityBadge = ({ priority }: PriorityBadgeProps) => {
	return <Badge className={cn('capitalize', priorityStyles[priority])}>{priority}</Badge>;
};
