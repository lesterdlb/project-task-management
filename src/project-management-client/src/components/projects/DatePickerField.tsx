import { format } from 'date-fns';
import { Calendar as CalendarIcon } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { Label } from '@/components/ui/label';

interface DatePickerFieldProps {
	label: string;
	value: Date | undefined;
	onChange: (date: Date | undefined) => void;
	error?: string;
	placeholder?: string;
	disabled?: boolean;
}

export const DatePickerField = ({
	label,
	value,
	onChange,
	error,
	placeholder = 'Pick a date',
	disabled = false,
}: DatePickerFieldProps) => {
	return (
		<div className='space-y-2'>
			<Label>{label}</Label>
			<Popover>
				<PopoverTrigger asChild>
					<Button
						variant='outline'
						className={cn(
							'w-full justify-start text-left font-normal',
							!value && 'text-muted-foreground',
							error && 'border-destructive'
						)}
						disabled={disabled}
					>
						<CalendarIcon className='mr-2 h-4 w-4' />
						{value ? format(value, 'PPP') : <span>{placeholder}</span>}
					</Button>
				</PopoverTrigger>
				<PopoverContent className='w-auto p-0'>
					<Calendar mode='single' selected={value} onSelect={onChange} autoFocus />
				</PopoverContent>
			</Popover>
			{error && <p className='text-sm text-destructive'>{error}</p>}
		</div>
	);
};
