import { z } from 'zod';
import { ProjectStatus, ProjectPriority } from '@/types/project.types';

export const projectFormSchema = z
	.object({
		name: z
			.string()
			.min(1, 'Name is required')
			.max(100, 'Name must be less than 100 characters'),
		description: z
			.string()
			.min(1, 'Description is required')
			.max(500, 'Description must be less than 500 characters'),
		startDate: z.string({ error: 'Start date is required' }).min(1, 'Start date is required'),
		endDate: z.string().optional(),
		status: z.enum([
			ProjectStatus.Planned,
			ProjectStatus.Active,
			ProjectStatus.Completed,
			ProjectStatus.Archived,
		]),
		priority: z.enum([ProjectPriority.Low, ProjectPriority.Medium, ProjectPriority.High]),
	})
	.refine(
		data => {
			if (!data.endDate) {
				return true;
			}
			return new Date(data.endDate) > new Date(data.startDate);
		},
		{
			message: 'End date must be after start date',
			path: ['endDate'],
		}
	);

export type ProjectFormData = z.infer<typeof projectFormSchema>;
