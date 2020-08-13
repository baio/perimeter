/// <reference types="cypress" />

declare namespace Cypress {
    interface Chainable {
        dataCy(
            value: string,
            selector?: string
        ): Chainable<JQuery<HTMLElement>>;

        formField(value: string): Chainable<JQuery<HTMLElement>>;

        rows(index?: number, cellIndex?: number): Chainable<JQuery<HTMLElement>>;

        submitButton(): Chainable<JQuery<HTMLElement>>;

        resetDb(): Chainable<Window>;

        reinitDb(): Chainable<Window>;

        stickyVariable(value?: string): Chainable<string>;
    }
}
