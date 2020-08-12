/// <reference types="cypress" />

declare namespace Cypress {
    interface Chainable {
        dataCy(
            value: string,
            selector?: string
        ): Chainable<JQuery<HTMLElement>>;

        formField(value: string): Chainable<JQuery<HTMLElement>>;

        submitButton(): Chainable<JQuery<HTMLElement>>;

        refreshDb(): Chainable<Window>;

        stickyVariable(value?: string): Chainable<string>;
    }
}
