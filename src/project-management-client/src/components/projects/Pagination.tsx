import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { type PaginationMetadata } from '@/types/api.types';

interface PaginationProps {
	metadata: PaginationMetadata;
	onPageChange: (page: number) => void;
}

export const Pagination = ({ metadata, onPageChange }: PaginationProps) => {
	const { currentPage, totalPages, hasPrevious, hasNext } = metadata;

	return (
		<div className='flex items-center justify-between'>
			<p className='text-sm text-muted-foreground'>
				Page {currentPage} of {totalPages}
			</p>
			<div className='flex gap-2'>
				<Button
					variant='outline'
					size='sm'
					onClick={() => onPageChange(currentPage - 1)}
					disabled={!hasPrevious}
				>
					<ChevronLeft className='h-4 w-4 mr-1' />
					Previous
				</Button>
				<Button
					variant='outline'
					size='sm'
					onClick={() => onPageChange(currentPage + 1)}
					disabled={!hasNext}
				>
					Next
					<ChevronRight className='h-4 w-4 ml-1' />
				</Button>
			</div>
		</div>
	);
};
