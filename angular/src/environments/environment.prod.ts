import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44314/',
  redirectUri: baseUrl,
  clientId: 'EHub_App',
  responseType: 'code',
  scope: 'offline_access EHub',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'EHub',
    logoUrl: 'assets/images/logo/logo-dark.png',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44314',
      rootNamespace: 'EHub',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge',
  },
} as Environment;
