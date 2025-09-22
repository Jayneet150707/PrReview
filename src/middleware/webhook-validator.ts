import { Request, Response, NextFunction } from 'express';
import crypto from 'crypto';
import { environment } from '../config/environment';
import { logger } from '../utils/logger';
import { createError } from './error-handler';

export const validateGitHubWebhook = (
  req: Request,
  res: Response,
  next: NextFunction
): void => {
  try {
    const signature = req.headers['x-hub-signature-256'] as string;
    const payload = JSON.stringify(req.body);
    
    if (!signature) {
      throw createError('Missing GitHub signature', 401);
    }

    const expectedSignature = `sha256=${crypto
      .createHmac('sha256', environment.githubWebhookSecret)
      .update(payload, 'utf8')
      .digest('hex')}`;

    const isValid = crypto.timingSafeEqual(
      Buffer.from(signature),
      Buffer.from(expectedSignature)
    );

    if (!isValid) {
      logger.warn('Invalid GitHub webhook signature', {
        receivedSignature: signature,
        expectedSignature
      });
      throw createError('Invalid signature', 401);
    }

    logger.debug('GitHub webhook signature validated successfully');
    next();
  } catch (error) {
    next(error);
  }
};

export const validateContentType = (
  req: Request,
  res: Response,
  next: NextFunction
): void => {
  const contentType = req.headers['content-type'];
  
  if (!contentType || !contentType.includes('application/json')) {
    return next(createError('Invalid content type. Expected application/json', 400));
  }
  
  next();
};

export const validateGitHubEvent = (
  req: Request,
  res: Response,
  next: NextFunction
): void => {
  const event = req.headers['x-github-event'] as string;
  
  if (!event) {
    return next(createError('Missing GitHub event header', 400));
  }
  
  // Store event type for later use
  req.body.githubEvent = event;
  
  logger.debug('GitHub event received', { event });
  next();
};

