import axios from 'axios';
import { encerrarSessao, obterToken } from './auth';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5041/api',
});

// Envia o token em toda requisição
api.interceptors.request.use((config) => {
  const token = obterToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Token expirado ou inválido: volta para a tela de login
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const rotaDeAuth = error.config?.url?.startsWith('/auth/');
    if (error.response?.status === 401 && !rotaDeAuth) {
      encerrarSessao();
    }
    return Promise.reject(error);
  },
);
