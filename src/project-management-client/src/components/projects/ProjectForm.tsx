import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { projectFormSchema, type ProjectFormData } from '@/schemas/projectSchema';
import { ProjectStatus, ProjectPriority } from '@/types/project.types';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from '@/components/ui/select';
import { DatePickerField } from './DatePickerField';

interface ProjectFormProps {
	onSubmit: (data: ProjectFormData) => void;
	defaultValues?: Partial<ProjectFormData>;
	isSubmitting: boolean;
	submitButtonText: string;
	onCancel: () => void;
}

export const ProjectForm = ({
	onSubmit,
	defaultValues,
	isSubmitting,
	submitButtonText,
	onCancel,
}: ProjectFormProps) => {
	const {
		register,
		handleSubmit,
		control,
		formState: { errors },
	} = useForm<ProjectFormData>({
		resolver: zodResolver(projectFormSchema),
		defaultValues: {
			status: ProjectStatus.Planned,
			priority: ProjectPriority.Medium,
			...defaultValues,
		},
	});
	return (
		<form onSubmit={handleSubmit(onSubmit)} className='space-y-4'>
			<div className='space-y-2'>
				<Label htmlFor='name'>Name</Label>
				<Input
					id='name'
					placeholder='Enter project name'
					{...register('name')}
					aria-invalid={errors.name ? 'true' : 'false'}
				/>
				{errors.name && <p className='text-sm text-destructive'>{errors.name.message}</p>}
			</div>
			<div className='space-y-2'>
				<Label htmlFor='description'>Description</Label>
				<Input
					id='description'
					placeholder='Enter project description'
					{...register('description')}
					aria-invalid={errors.description ? 'true' : 'false'}
				/>
				{errors.description && (
					<p className='text-sm text-destructive'>{errors.description.message}</p>
				)}
			</div>
			<Controller
				control={control}
				name='startDate'
				render={({ field }) => (
					<DatePickerField
						label='Start Date'
						value={field.value ? new Date(field.value) : undefined}
						onChange={date => field.onChange(date?.toISOString().split('T')[0])}
						error={errors.startDate?.message}
					/>
				)}
			/>
			<Controller
				control={control}
				name='endDate'
				render={({ field }) => (
					<DatePickerField
						label='End Date (Optional)'
						value={field.value ? new Date(field.value) : undefined}
						onChange={date => field.onChange(date?.toISOString().split('T')[0])}
						error={errors.endDate?.message}
						placeholder='Select end date'
					/>
				)}
			/>
			<div className='space-y-2'>
				<Label htmlFor='status'>Status</Label>
				<Controller
					control={control}
					name='status'
					render={({ field }) => (
						<Select onValueChange={field.onChange} value={field.value}>
							<SelectTrigger>
								<SelectValue placeholder='Select status' />
							</SelectTrigger>
							<SelectContent>
								<SelectItem value={ProjectStatus.Planned}>Planned</SelectItem>
								<SelectItem value={ProjectStatus.Active}>Active</SelectItem>
								<SelectItem value={ProjectStatus.Completed}>Completed</SelectItem>
								<SelectItem value={ProjectStatus.Archived}>Archived</SelectItem>
							</SelectContent>
						</Select>
					)}
				/>
				{errors.status && (
					<p className='text-sm text-destructive'>{errors.status.message}</p>
				)}
			</div>
			<div className='space-y-2'>
				<Label htmlFor='priority'>Priority</Label>
				<Controller
					control={control}
					name='priority'
					render={({ field }) => (
						<Select onValueChange={field.onChange} value={field.value}>
							<SelectTrigger>
								<SelectValue placeholder='Select priority' />
							</SelectTrigger>
							<SelectContent>
								<SelectItem value={ProjectPriority.Low}>Low</SelectItem>
								<SelectItem value={ProjectPriority.Medium}>Medium</SelectItem>
								<SelectItem value={ProjectPriority.High}>High</SelectItem>
							</SelectContent>
						</Select>
					)}
				/>
				{errors.priority && (
					<p className='text-sm text-destructive'>{errors.priority.message}</p>
				)}
			</div>
			<div className='flex justify-end gap-3 pt-4'>
				<Button type='button' variant='outline' onClick={onCancel} disabled={isSubmitting}>
					Cancel
				</Button>
				<Button type='submit' disabled={isSubmitting}>
					{isSubmitting ? 'Saving...' : submitButtonText}
				</Button>
			</div>
		</form>
	);
};
