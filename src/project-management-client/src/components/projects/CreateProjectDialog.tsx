import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { ProjectForm } from './ProjectForm';
import { createProject } from '@/services/projectService';
import { type ProjectFormData } from '@/schemas/projectSchema';
import { type ApiError } from '@/types/api.types';

interface CreateProjectDialogProps {
	open: boolean;
	onOpenChange: (open: boolean) => void;
	onSuccess: () => void;
}

export const CreateProjectDialog = ({
	open,
	onOpenChange,
	onSuccess,
}: CreateProjectDialogProps) => {
	const [isSubmitting, setIsSubmitting] = useState(false);
	const [apiError, setApiError] = useState<string | null>(null);

	const handleSubmit = async (data: ProjectFormData) => {
		setIsSubmitting(true);
		setApiError(null);

		try {
			await createProject({
				name: data.name,
				description: data.description,
				startDate: new Date(data.startDate).toISOString(),
				endDate: data.endDate ? new Date(data.endDate).toISOString() : undefined,
				status: data.status,
				priority: data.priority,
			});

			onOpenChange(false);
			onSuccess();
		} catch (error) {
			const apiErr = error as ApiError;

			if (apiErr.status === 400 && apiErr.errors) {
				const errorMessages = Object.entries(apiErr.errors)
					.map(([field, messages]) => {
						const fieldName = field.split('.').pop() || field;
						return `${fieldName}: ${messages.join(', ')}`;
					})
					.join('\n');
				setApiError(errorMessages);
			} else {
				setApiError(apiErr.detail || 'Failed to create project. Please try again.');
			}
		} finally {
			setIsSubmitting(false);
		}
	};

	return (
		<Dialog open={open} onOpenChange={onOpenChange}>
			<DialogContent className='max-w-2xl max-h-[90vh] overflow-y-auto'>
				<DialogHeader>
					<DialogTitle>Create New Project</DialogTitle>
				</DialogHeader>
				{apiError && (
					<Alert variant='destructive'>
						<AlertDescription className='whitespace-pre-line'>
							{apiError}
						</AlertDescription>
					</Alert>
				)}
				<ProjectForm
					onSubmit={handleSubmit}
					isSubmitting={isSubmitting}
					submitButtonText='Create Project'
					onCancel={() => onOpenChange(false)}
				/>
			</DialogContent>
		</Dialog>
	);
};
