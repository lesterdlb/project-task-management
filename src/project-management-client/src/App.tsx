import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from '@/contexts/AuthProvider';
import Layout from '@/components/layout/Layout';

import HomePage from '@/pages/HomePage';
import LoginPage from './pages/LoginPage';
import ProjectsPage from '@/pages/ProjectsPage';

import { ROUTES } from '@/lib/constants';

const App = () => {
	return (
		<BrowserRouter>
			<AuthProvider>
				<Layout>
					<Routes>
						<Route path={ROUTES.HOME} element={<HomePage />} />
						<Route path={ROUTES.LOGIN} element={<LoginPage />} />
						<Route path={ROUTES.PROJECTS} element={<ProjectsPage />} />
					</Routes>
				</Layout>
			</AuthProvider>
		</BrowserRouter>
	);
};

export default App;
