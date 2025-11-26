export const formatDate = (dateString: string | null | undefined): string => {
	if (!dateString) {
		return 'N/A';
	}

	const date = new Date(dateString);

	return new Intl.DateTimeFormat('en-US', {
		year: 'numeric',
		month: 'short',
		day: 'numeric',
	}).format(date);
};

export const formatDateForInput = (dateString: string | null | undefined): string => {
	if (!dateString) {
		return '';
	}

	return dateString.split('T')[0];
};

export const toISOString = (date: Date): string => {
	return date.toISOString();
};
