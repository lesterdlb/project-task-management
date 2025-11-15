import { Link } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { ROUTES } from '@/lib/constants';

const HomePage = () => {
	const { isAuthenticated, user } = useAuth();

	if (isAuthenticated && user) {
		return (
			<div className='container mx-auto py-8'>
				<Card className='max-w-2xl mx-auto'>
					<CardHeader>
						<CardTitle className='text-3xl'>Welcome back, {user.fullName}!</CardTitle>
						<CardDescription>
							Manage your projects and tasks efficiently
						</CardDescription>
					</CardHeader>
					<CardContent className='space-y-4'>
						<p>
							You're logged in as <strong>{user.email}</strong>
						</p>
						<div className='flex gap-4'>
							<Button asChild>
								<Link to={ROUTES.PROJECTS}>View Projects</Link>
							</Button>
							<Button variant='outline' asChild>
								<Link to={ROUTES.PROFILE}>My Profile</Link>
							</Button>
						</div>
					</CardContent>
				</Card>
			</div>
		);
	}

	return (
		<div className='container mx-auto py-8'>
			<Card className='max-w-2xl mx-auto'>
				<CardHeader>
					<CardTitle className='text-3xl'>Welcome to Project Management</CardTitle>
					<CardDescription>
						Organize your projects, collaborate with your team, and track progress
					</CardDescription>
				</CardHeader>
				<CardContent className='space-y-4'>
					<p>Get started by creating an account or logging in to access your projects.</p>
					<div className='flex gap-4'>
						<Button asChild>
							<Link to={ROUTES.REGISTER}>Create Account</Link>
						</Button>
						<Button variant='outline' asChild>
							<Link to={ROUTES.LOGIN}>Sign In</Link>
						</Button>
					</div>
				</CardContent>
			</Card>
		</div>
	);
};

export default HomePage;
