import React from 'react';
import Header from './Header';

interface LayoutProps {
	children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
	return (
		<div className='min-h-screen bg-background flex flex-col'>
			<Header />
			<main className='flex-1'>
				<div className='container mx-auto px-4 py-8'>{children}</div>
			</main>
			<footer className='border-t py-6 md:py-8'>
				<div className='container mx-auto px-4 text-center text-sm text-muted-foreground'>
					<p>
						&copy; {new Date().getFullYear()} Project Management. All rights reserved.
					</p>
				</div>
			</footer>
		</div>
	);
};

export default Layout;
