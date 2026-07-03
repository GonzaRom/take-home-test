import { HttpErrorResponse } from '@angular/common/http';

interface ProblemDetailsError {
  readonly title?: string;
  readonly detail?: string;
  readonly errors?: Record<string, readonly string[]>;
}

export function extractApiErrorMessage(error: unknown, fallbackMessage: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallbackMessage;
  }

  if (typeof error.error === 'string' && error.error.trim().length > 0) {
    return error.error;
  }

  if (error.error && typeof error.error === 'object') {
    const problemDetails = error.error as ProblemDetailsError;
    const validationMessages = problemDetails.errors
      ? Object.values(problemDetails.errors)
          .flat()
          .filter((message): message is string => typeof message === 'string' && message.trim().length > 0)
      : [];

    if (validationMessages.length > 0) {
      return validationMessages.join(' ');
    }

    if (problemDetails.detail && problemDetails.detail.trim().length > 0) {
      return problemDetails.detail;
    }

    if (problemDetails.title && problemDetails.title.trim().length > 0) {
      return problemDetails.title;
    }
  }

  if (error.status === 404) {
    return 'The requested loan could not be found.';
  }

  return fallbackMessage;
}
