import { useEffect, useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAuth } from '@/hooks/useAuth';
import {
	Card,
	CardContent,
	CardDescription,
	CardFooter,
	CardHeader,
	CardTitle,
} from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { ROUTES } from '@/lib/constants';
import { type ApiError } from '@/types/api.types';

const loginSchema = z.object({
	email: z.email('Invalid email address'),
	password: z.string().min(6, 'Password must be at least 6 characters'),
});

type LoginFormData = z.infer<typeof loginSchema>;

interface LocationState {
	from?: {
		pathname: string;
	};
}

const LoginPage = () => {
	const { login, isAuthenticated } = useAuth();
	const navigate = useNavigate();
	const location = useLocation();
	const [apiError, setApiError] = useState<string | null>(null);
	const [isSubmitting, setIsSubmitting] = useState(false);

	const from = (location.state as LocationState)?.from?.pathname || ROUTES.HOME;

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<LoginFormData>({
		resolver: zodResolver(loginSchema),
	});

	const onSubmit = async (data: LoginFormData) => {
		setIsSubmitting(true);
		setApiError(null);

		try {
			await login(data);
		} catch (error) {
			const apiErr = error as ApiError;
			if (apiErr.status === 401) {
				setApiError('Invalid email or password. Please try again.');
			} else if (apiErr.detail) {
				setApiError(apiErr.detail);
			} else {
				setApiError('An error occurred during login. Please try again.');
			}
		} finally {
			setIsSubmitting(false);
		}
	};

	useEffect(() => {
		if (isAuthenticated) {
			navigate(from, { replace: true });
		}
	}, [isAuthenticated, navigate, from]);

	return (
		<div className='container mx-auto py-8 flex items-center justify-center min-h-[calc(100vh-200px)]'>
			<Card className='w-full max-w-md'>
				<CardHeader>
					<CardTitle className='text-2xl'>Sign In</CardTitle>
					<CardDescription>Enter your credentials to access your account</CardDescription>
				</CardHeader>
				<form onSubmit={handleSubmit(onSubmit)}>
					<CardContent className='space-y-4'>
						{apiError && (
							<Alert variant='destructive'>
								<AlertDescription>{apiError}</AlertDescription>
							</Alert>
						)}

						<div className='space-y-2'>
							<Label htmlFor='email'>Email</Label>
							<Input
								id='email'
								type='email'
								placeholder='your.email@example.com'
								{...register('email')}
								aria-invalid={errors.email ? 'true' : 'false'}
							/>
							{errors.email && (
								<p className='text-sm text-destructive'>{errors.email.message}</p>
							)}
						</div>

						<div className='space-y-2'>
							<Label htmlFor='password'>Password</Label>
							<Input
								id='password'
								type='password'
								placeholder='Enter your password'
								{...register('password')}
								aria-invalid={errors.password ? 'true' : 'false'}
							/>
							{errors.password && (
								<p className='text-sm text-destructive'>
									{errors.password.message}
								</p>
							)}
						</div>
					</CardContent>
					<CardFooter className='flex flex-col space-y-4'>
						<Button type='submit' className='w-full' disabled={isSubmitting}>
							{isSubmitting ? 'Signing in...' : 'Sign In'}
						</Button>
						<p className='text-sm text-muted-foreground text-center'>
							Don't have an account?{' '}
							<Link to={ROUTES.REGISTER} className='text-primary hover:underline'>
								Create one
							</Link>
						</p>
					</CardFooter>
				</form>
			</Card>
		</div>
	);
};

export default LoginPage;
