import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface PaginationProps {
	page: number;
	totalPages: number;
	hasPreviousPage: boolean;
	hasNextPage: boolean;
	onPageChange: (page: number) => void;
}

export const Pagination = ({
	page,
	totalPages,
	hasPreviousPage,
	hasNextPage,
	onPageChange,
}: PaginationProps) => {
	return (
		<div className='flex items-center justify-between'>
			<p className='text-sm text-muted-foreground'>
				Page {page} of {totalPages}
			</p>
			<div className='flex gap-2'>
				<Button
					variant='outline'
					size='sm'
					onClick={() => onPageChange(page - 1)}
					disabled={!hasPreviousPage}
				>
					<ChevronLeft className='h-4 w-4 mr-1' />
					Previous
				</Button>
				<Button
					variant='outline'
					size='sm'
					onClick={() => onPageChange(page + 1)}
					disabled={!hasNextPage}
				>
					Next
					<ChevronRight className='h-4 w-4 ml-1' />
				</Button>
			</div>
		</div>
	);
};
