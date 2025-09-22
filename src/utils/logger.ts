import winston from 'winston';
import { environment } from '../config/environment';

const logFormat = environment.logFormat === 'json' 
  ? winston.format.combine(
      winston.format.timestamp(),
      winston.format.errors({ stack: true }),
      winston.format.json()
    )
  : winston.format.combine(
      winston.format.timestamp(),
      winston.format.errors({ stack: true }),
      winston.format.simple()
    );

export const logger = winston.createLogger({
  level: environment.logLevel,
  format: logFormat,
  defaultMeta: { service: 'pr-reviewer-bot' },
  transports: [
    new winston.transports.Console({
      format: environment.nodeEnv === 'development' 
        ? winston.format.combine(
            winston.format.colorize(),
            winston.format.simple()
          )
        : logFormat
    })
  ]
});

// Add file logging in production
if (environment.nodeEnv === 'production') {
  logger.add(new winston.transports.File({
    filename: 'logs/error.log',
    level: 'error'
  }));
  
  logger.add(new winston.transports.File({
    filename: 'logs/combined.log'
  }));
}

