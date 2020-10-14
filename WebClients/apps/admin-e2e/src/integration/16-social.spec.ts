// tslint:disable: no-unused-expression
describe('social', () => {
    const UPDATED_NAME = 'updated name';
    before(() => cy.reinitDb(true));

    before(() => {
        cy.visit('/domains/2/apps');
        cy.dataCy('social-menu-item').click();
    });

    it('social should be open', () => {
        cy.url().should('include', 'social');
    });

    describe('list', () => {
        it('rows count gt 0', () => {
            cy.rows().should('have.length.gt', 0);
        });
    });

    describe('enable', () => {
        it('enable 1st social connection', () => {
            cy.rows(0)
                .click()
                .formToggle('isEnabled')
                .click()
                .formField('clientId')
                .type('abcd')
                .formField('clientSecret')
                .type('123')
                .formSelect('attributes')
                .type('some{enter}');

            cy.submitButton().click({ force: true });

            cy.get('.ant-drawer').should('not.exist');
        });

        it('update 1st social connection', () => {
            cy.rows(0).click().formField('clientId').clear().type('abcdef');

            cy.submitButton().click();

            cy.get('.ant-drawer').should('not.exist');
        });

        it('disable 1st social connection', () => {
            cy.rows(0).click().formToggle('isEnabled').click();

            cy.submitButton().click();

            cy.get('.ant-drawer').should('not.exist');
        });
    });
});
