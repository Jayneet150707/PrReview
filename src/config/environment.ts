import Joi from 'joi';

interface Environment {
  // Server
  port: number;
  nodeEnv: string;
  
  // GitHub
  githubToken: string;
  githubWebhookSecret: string;
  githubAppId?: string;
  githubPrivateKey?: string;
  
  // OpenAI
  openaiApiKey: string;
  openaiModel: string;
  openaiMaxTokens: number;
  
  // Review Configuration
  targetBranch: string;
  reviewEnabled: boolean;
  autoApproveEnabled: boolean;
  maxFilesToReview: number;
  maxDiffSize: number;
  
  // Logging
  logLevel: string;
  logFormat: string;
  
  // Rate Limiting
  openaiRateLimitRpm: number;
  githubRateLimitRph: number;
}

const schema = Joi.object({
  PORT: Joi.number().default(3000),
  NODE_ENV: Joi.string().valid('development', 'staging', 'production').default('development'),
  
  GITHUB_TOKEN: Joi.string().required(),
  GITHUB_WEBHOOK_SECRET: Joi.string().required(),
  GITHUB_APP_ID: Joi.string().optional(),
  GITHUB_PRIVATE_KEY: Joi.string().optional(),
  
  OPENAI_API_KEY: Joi.string().required(),
  OPENAI_MODEL: Joi.string().default('gpt-4'),
  OPENAI_MAX_TOKENS: Joi.number().default(2000),
  
  TARGET_BRANCH: Joi.string().default('beta'),
  REVIEW_ENABLED: Joi.boolean().default(true),
  AUTO_APPROVE_ENABLED: Joi.boolean().default(false),
  MAX_FILES_TO_REVIEW: Joi.number().default(20),
  MAX_DIFF_SIZE: Joi.number().default(10000),
  
  LOG_LEVEL: Joi.string().valid('error', 'warn', 'info', 'debug').default('info'),
  LOG_FORMAT: Joi.string().valid('json', 'simple').default('json'),
  
  OPENAI_RATE_LIMIT_RPM: Joi.number().default(60),
  GITHUB_RATE_LIMIT_RPH: Joi.number().default(5000)
});

const { error, value } = schema.validate(process.env, { allowUnknown: true });

if (error) {
  throw new Error(`Environment validation error: ${error.message}`);
}

export const environment: Environment = {
  port: value.PORT,
  nodeEnv: value.NODE_ENV,
  
  githubToken: value.GITHUB_TOKEN,
  githubWebhookSecret: value.GITHUB_WEBHOOK_SECRET,
  githubAppId: value.GITHUB_APP_ID,
  githubPrivateKey: value.GITHUB_PRIVATE_KEY,
  
  openaiApiKey: value.OPENAI_API_KEY,
  openaiModel: value.OPENAI_MODEL,
  openaiMaxTokens: value.OPENAI_MAX_TOKENS,
  
  targetBranch: value.TARGET_BRANCH,
  reviewEnabled: value.REVIEW_ENABLED,
  autoApproveEnabled: value.AUTO_APPROVE_ENABLED,
  maxFilesToReview: value.MAX_FILES_TO_REVIEW,
  maxDiffSize: value.MAX_DIFF_SIZE,
  
  logLevel: value.LOG_LEVEL,
  logFormat: value.LOG_FORMAT,
  
  openaiRateLimitRpm: value.OPENAI_RATE_LIMIT_RPM,
  githubRateLimitRph: value.GITHUB_RATE_LIMIT_RPH
};

