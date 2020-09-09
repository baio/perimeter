import * as k8s from '@pulumi/kubernetes';

// prr-web-admin

const prrWebAdminLabels = { app: 'prr-web-admin' };
const prrWebAdminDeployment = new k8s.apps.v1.Deployment('prr-web-admin', {
    spec: {
        selector: { matchLabels: prrWebAdminLabels },
        replicas: 1,
        template: {
            metadata: { labels: prrWebAdminLabels },
            spec: {
                containers: [
                    { name: 'prr-web-admin', image: 'baio/prr-web-admin' },
                ],
            },
        },
    },
});
export const prrWebAdminDeploymentName = prrWebAdminDeployment.metadata.name;

const prrWebAdminLoadBalancer = new k8s.core.v1.Service('prr-web-admin', {
    metadata: {
        name: 'prr-web-admin',
        labels: {
            name: 'prr-web-admin',
        },
    },
    spec: {
        type: 'LoadBalancer',
        ports: [
            {
                port: 8071,
                targetPort: 80,
            },
        ],
        selector: prrWebAdminLabels,
    },
});

export const prrWebAdminLoadBalancerUrn = prrWebAdminLoadBalancer.urn;

// prr-api

const prrApiLabels = { app: 'prr-api' };
const prrApiDeployment = new k8s.apps.v1.Deployment('prr-api', {
    spec: {
        selector: { matchLabels: prrApiLabels },
        replicas: 1,
        template: {
            metadata: { labels: prrApiLabels },
            spec: {
                containers: [
                    {
                        name: 'prr-api',
                        image: 'baio/prr-api',
                        env: [
                            { name: 'ASPNETCORE_ENVIRONMENT', value: 'STAGE' },
                        ],
                    },
                ],
            },
        },
    },
});
export const prrApiDeploymentName = prrApiDeployment.metadata.name;

const prrApiLoadBalancer = new k8s.core.v1.Service('prr-api', {
    metadata: {
        name: 'prr-api',
        labels: {
            name: 'prr-api',
        },
    },
    spec: {
        type: 'ClusterIP',
        ports: [
            {
                port: 80,
                targetPort: 80,
            },
        ],
        selector: prrApiLabels,
    },
});

export const prrApiLoadBalancerUrn = prrApiLoadBalancer.urn;
