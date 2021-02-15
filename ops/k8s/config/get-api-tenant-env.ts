import * as pulumi from '@pulumi/pulumi';
import { getApiAuthEnv } from './get-api-auth-env';

export const getApiTenantEnv = getApiAuthEnv;