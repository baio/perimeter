import * as k8s from '@pulumi/kubernetes';

const prrWebAdminLabels = { app: 'prr-web-admin' };
const deployment = new k8s.apps.v1.Deployment('prr-web-admin', {
    spec: {
        selector: { matchLabels: prrWebAdminLabels },
        replicas: 1,
        template: {
            metadata: { labels: prrWebAdminLabels },
            spec: {
                containers: [{ name: 'prr-web-admin', image: 'baio/prr-web-admin' }],
            },
        },
    },
});
export const prrWebAdminDeploymentName = deployment.metadata.name;

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
                port: 8070,
                targetPort: 80,
            },
        ],
        selector: prrWebAdminLabels,
    },
});

export const prrWebAdminLoadBalancerUrn = prrWebAdminLoadBalancer.urn;