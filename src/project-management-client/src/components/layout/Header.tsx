import { Link } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { Button } from '@/components/ui/button';
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuLabel,
	DropdownMenuSeparator,
	DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { ROUTES, APP_NAME } from '@/lib/constants';
import { Briefcase, LogOut, User } from 'lucide-react';

const Header = () => {
	const { isAuthenticated, user, logout } = useAuth();

	const getInitials = (fullName: string) => {
		const names = fullName.split(' ');
		if (names.length >= 2) {
			return `${names[0][0]}${names[names.length - 1][0]}`.toUpperCase();
		}
		return fullName.slice(0, 2).toUpperCase();
	};

	return (
		<header className='border-b bg-background sticky top-0 z-50'>
			<div className='container mx-auto flex h-16 items-center justify-between px-4'>
				<Link
					to={ROUTES.HOME}
					className='flex items-center gap-2 hover:opacity-80 transition-opacity'
				>
					<Briefcase className='h-6 w-6 text-primary' />
					<span className='text-xl font-bold'>{APP_NAME}</span>
				</Link>

				{isAuthenticated && (
					<nav className='hidden md:flex items-center gap-6'>
						<Link
							to={ROUTES.HOME}
							className='text-sm font-medium text-muted-foreground hover:text-foreground transition-colors'
						>
							Home
						</Link>
						<Link
							to={ROUTES.PROJECTS}
							className='text-sm font-medium text-muted-foreground hover:text-foreground transition-colors'
						>
							Projects
						</Link>
						<Link
							to={ROUTES.PROFILE}
							className='text-sm font-medium text-muted-foreground hover:text-foreground transition-colors'
						>
							Profile
						</Link>
					</nav>
				)}

				<div className='flex items-center gap-4'>
					{isAuthenticated && user ? (
						<DropdownMenu>
							<DropdownMenuTrigger asChild>
								<Button variant='ghost' className='relative h-10 w-10 rounded-full'>
									<Avatar className='h-10 w-10'>
										<AvatarFallback className='bg-primary text-primary-foreground'>
											{getInitials(user.fullName)}
										</AvatarFallback>
									</Avatar>
								</Button>
							</DropdownMenuTrigger>
							<DropdownMenuContent className='w-56' align='end' forceMount>
								<DropdownMenuLabel className='font-normal'>
									<div className='flex flex-col space-y-1'>
										<p className='text-sm font-medium leading-none'>
											{user.fullName}
										</p>
										<p className='text-xs leading-none text-muted-foreground'>
											{user.email}
										</p>
									</div>
								</DropdownMenuLabel>
								<DropdownMenuSeparator />
								<DropdownMenuItem asChild>
									<Link to={ROUTES.PROFILE} className='cursor-pointer'>
										<User className='mr-2 h-4 w-4' />
										<span>Profile</span>
									</Link>
								</DropdownMenuItem>
								<DropdownMenuSeparator />
								<DropdownMenuItem
									onClick={logout}
									className='cursor-pointer text-destructive'
								>
									<LogOut className='mr-2 h-4 w-4' />
									<span>Log out</span>
								</DropdownMenuItem>
							</DropdownMenuContent>
						</DropdownMenu>
					) : (
						<div className='flex items-center gap-2'>
							<Button variant='ghost' asChild>
								<Link to={ROUTES.LOGIN}>Sign In</Link>
							</Button>
							<Button asChild>
								<Link to={ROUTES.REGISTER}>Sign Up</Link>
							</Button>
						</div>
					)}
				</div>
			</div>
		</header>
	);
};

export default Header;
