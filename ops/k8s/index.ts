import { appConfig } from './app-config';
import { createK8sCluster } from './create-k8s-cluster';

// https://www.pulumi.com/docs/reference/pkg/docker/image/

const version = 'v0.21';

export const cluster = createK8sCluster(version, appConfig);

/*
export const apiImageName = cluster.apiImageName;
export const apiDeploymentName = cluster.apiDeploymentName;
export const apiLoadBalancerUrn = cluster.apiLoadBalancerUrn;
*/
