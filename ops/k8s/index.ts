import { config } from './config';
import { createApiImage } from './create-api-image';

// https://www.pulumi.com/docs/reference/pkg/docker/image/

const version = 'v0.21';

export const apiImageName = createApiImage(version, config.registry);
