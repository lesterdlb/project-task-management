import { Link, useLocation } from 'react-router-dom';
import { cn } from '@/lib/utils';
import { ROUTES } from '@/lib/constants';

interface NavItem {
	label: string;
	href: string;
}

const navItems: NavItem[] = [
	{ label: 'Home', href: ROUTES.HOME },
	{ label: 'Projects', href: ROUTES.PROJECTS },
	{ label: 'Profile', href: ROUTES.PROFILE },
];

const Navigation = () => {
	const location = useLocation();

	return (
		<nav className='flex items-center gap-6'>
			{navItems.map(item => (
				<Link
					key={item.href}
					to={item.href}
					className={cn(
						'text-sm font-medium transition-colors hover:text-foreground',
						location.pathname === item.href
							? 'text-foreground'
							: 'text-muted-foreground'
					)}
				>
					{item.label}
				</Link>
			))}
		</nav>
	);
};

export default Navigation;
