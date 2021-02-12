import { createDeployment } from './deployment';
import { createLoadBalancer } from './load-balancer';
import { createNodePorts } from './node-ports';

export const create = () => {
    const name = 'seq';
    const deployment = createDeployment(name);
    // const loadBalancer = createLoadBalancer(name);
    const nodePorts = createNodePorts(name);
    return {
        deployment: deployment.urn,
        // loadBalancer: loadBalancer.urn,
        nodePorts: nodePorts.urn,
    };
};
