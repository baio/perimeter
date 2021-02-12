import { createDeployment } from './deployment';
import { createLoadBalancer } from './load-balancer';
import { createNodePorts } from './node-ports';

export const create = () => {
    const name = 'jaeger';
    const deployment = createDeployment(name);
    const nodePorts = createNodePorts(name);
    // const loadBalancer = createLoadBalancer(name);
    return {
        deployment: deployment.urn,
        nodePorts: nodePorts.urn,
        // loadBalancer: loadBalancer.urn,
    };
};
