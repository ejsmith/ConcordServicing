import { defineConfig } from 'vite';
import { sveltekit } from '@sveltejs/kit/vite';

import fs from 'fs';
import path from 'path';
import { execSync } from 'child_process';

export default defineConfig({
	plugins: [sveltekit()],

	server: {
		port: 3000,
		strictPort: true,
		https: generateCerts(),
		proxy: {
			// proxy API requests to the ASP.NET backend
			'/api': {
				changeOrigin: true,
				secure: false,
				rewrite: (path) => path.replace(/^\/api/, '/api'),
				target: getTarget()
            },
            '/metrics': {
                changeOrigin: true,
                secure: false,
                rewrite: (path) => path.replace(/^\/metrics/, '/metrics'),
                target: getTarget()
            },
            '/api/events': {
				changeOrigin: true,
                secure: false,
				rewrite: (path) => path.replace(/^\/api/, '/api'),
                target: getWsTarget(),
                ws: true,
            },
			'/swagger': {
				changeOrigin: true,
				secure: false,
				rewrite: (path) => path.replace(/^\/swagger/, '/swagger'),
				target: getTarget()
			},
			'/_framework': {
				changeOrigin: true,
				secure: false,
				rewrite: (path) => path.replace(/^\/_framework/, '/_framework'),
				target: getTarget()
			},
			'/_vs': {
				changeOrigin: true,
				secure: false,
				rewrite: (path) => path.replace(/^\/_vs/, '/_vs'),
				target: getTarget()
			}
		}
	},

	css: {
		preprocessorOptions: {
			scss: {
				additionalData: '@use "src/variables.scss" as *;'
			}
		}
	}
});

// target taken from src/setupProxy.js in ASP.NET React template
function getTarget() {
	return process.env.ASPNETCORE_HTTPS_PORT
		? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
		: process.env.ASPNETCORE_URLS
			? process.env.ASPNETCORE_URLS.split(';')[0]
			: 'http://localhost:7049'
}

// target taken from src/setupProxy.js in ASP.NET React template
function getWsTarget() {
	var target = process.env.ASPNETCORE_HTTPS_PORT
		? `wss://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
		: process.env.ASPNETCORE_URLS
			? process.env.ASPNETCORE_URLS.split(';')[0]
			: 'ws://localhost:7049';

    return target.replace('https://', 'wss://').replace('http://', 'ws://');
}

/** Function taken from aspnetcore-https.js in ASP.NET React template */
function generateCerts() {
	const baseFolder =
		process.env.APPDATA !== undefined && process.env.APPDATA !== ''
			? `${process.env.APPDATA}/ASP.NET/https`
			: `${process.env.HOME}/.aspnet/https`;
	const certificateArg = process.argv
		.map((arg) => arg.match(/--name=(?<value>.+)/i))
		.filter(Boolean)[0];
	const certificateName = certificateArg
		? certificateArg.groups.value
		: process.env.npm_package_name;

	if (!certificateName) {
		console.error(
			'Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.'
		);
		process.exit(-1);
	}

	const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
	const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

	if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
		const outp = execSync(
			'dotnet ' +
				[
					'dev-certs',
					'https',
					'--export-path',
					certFilePath,
					'--format',
					'Pem',
					'--no-password'
				].join(' ')
		);
		console.log(outp.toString());
	}

	return {
		cert: fs.readFileSync(certFilePath, 'utf8'),
		key: fs.readFileSync(keyFilePath, 'utf8')
	};
}
