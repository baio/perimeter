/// <reference types="cypress" />

declare namespace Cypress {
    interface Chainable {
        dataCy(
            value: string,
            selector?: string
        ): Chainable<JQuery<HTMLElement>>;

        formField(value: string): Chainable<JQuery<HTMLElement>>;

        formSelect(value: string): Chainable<JQuery<HTMLElement>>;

        formSelectChoose(
            value: string,
            index: number
        ): Chainable<JQuery<HTMLElement>>;
        
        formSelectSingle(
            value: string,
            index: number
        ): Chainable<JQuery<HTMLElement>>;

        rowCommand(
            rowIndex: number,
            commandIndex: number
        ): Chainable<JQuery<HTMLElement>>;

        rows(
            index?: number,
            cellIndex?: number
        ): Chainable<JQuery<HTMLElement>>;

        cancelButton(): Chainable<JQuery<HTMLElement>>;

        submitButton(): Chainable<JQuery<HTMLElement>>;

        confirmNoButton(): Chainable<JQuery<HTMLElement>>;

        confirmYesButton(): Chainable<JQuery<HTMLElement>>;

        resetDb(): Chainable<Window>;

        reinitDb(): Chainable<Window>;

        login(): Chainable<Window>;

        stickyVariable(value?: string): Chainable<string>;
    }
}
