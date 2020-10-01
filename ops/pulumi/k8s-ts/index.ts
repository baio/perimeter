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
                    { name: 'prr-web-admin', image: 'baio/prr-web-admin:v0.12' },
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
                port: 80,
                targetPort: 80,
            },
        ],
        selector: prrWebAdminLabels,
    },
});

export const prrWebAdminLoadBalancerUrn = prrWebAdminLoadBalancer.urn;

// prr-web-idp
/*
const prrWebIdpLabels = { app: 'prr-web-idp' };
const prrWebIdpDeployment = new k8s.apps.v1.Deployment('prr-web-idp', {
    spec: {
        selector: { matchLabels: prrWebIdpLabels },
        replicas: 1,
        template: {
            metadata: { labels: prrWebIdpLabels },
            spec: {
                containers: [
                    { name: 'prr-web-idp', image: 'baio/prr-web-idp' },
                ],
            },
        },
    },
});
export const prrWebIdpDeploymentName = prrWebIdpDeployment.metadata.name;

const prrWebIdpLoadBalancer = new k8s.core.v1.Service('prr-web-idp', {
    metadata: {
        name: 'prr-web-idp',
        labels: {
            name: 'prr-web-idp',
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
        selector: prrWebIdpLabels,
    },
});

export const prrWebIdpLoadBalancerUrn = prrWebIdpLoadBalancer.urn;

*/
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
                        image: 'baio/prr-api:v0.20',
                        env: [
                            { name: 'ASPNETCORE_ENVIRONMENT', value: 'STAGE' },
                            { name : 'MailSender__Project__BaseUrl', value: 'https://perimeter.azurefd.net'},
                            { name : 'MailSender__Project__Name', value: 'Perimeter (stage)'},
                            { name : 'MailSender__FromEmail', value: 'maxp@scal.io'},
                            { name : 'MailSender__FromName', value: 'Perimeter (stage)'},
                            { name : 'SendGridApiKey', value: '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'}
                        ],
                    },
                ],
            },
        },
    },
});
export const prrApiDeploymentName = prrApiDeployment.metadata.name;

/*
const prrApiClusterIP = new k8s.core.v1.Service('prr-api-cip', {
    metadata: {
        name: 'prr-api-cip',
        labels: {
            name: 'prr-api-cip',
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

export const prrApiClusterIPUrn = prrApiClusterIP.urn;
*/

const prrIpLoadBalancer = new k8s.core.v1.Service('prr-api', {
    metadata: {
        name: 'prr-api',
        labels: {
            name: 'prr-api',
        },
    },
    spec: {
        type: 'LoadBalancer',
        ports: [
            {
                port: 80,
                targetPort: 80,
            },
        ],
        selector: prrApiLabels,
    },
});

export const prrIpLoadBalancerUrn = prrIpLoadBalancer.urn;
