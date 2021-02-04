import { appConfig } from './app-config';
import { createK8sCluster } from './create-k8s-cluster';

// https://www.pulumi.com/docs/reference/pkg/docker/image/

const version = 'v0.22';

export const cluster = createK8sCluster(version, appConfig);
