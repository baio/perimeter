import { clearLocalStorage } from "./_setup";

// tslint:disable: no-unused-expression
describe('apis', () => {
    const UPDATED_NAME = 'updated name';
    before(() => cy.reinitDb(true));

    before(() => {
        cy.visit('/domains/2/apps');
        cy.dataCy('apis-menu-item').click();
    });

    it('app should be open', () => {
        cy.url().should('include', 'apis');
    });

    describe.skip('edit', () => {
        it('load app edit form data', () => {
            cy.rows(0).click();
            cy.url().should('match', /\/domains\/\d+\/apis\/\d+/);
            cy.formField('name').should((input) => {
                const val = input.val();
                expect(val).be.not.empty;
            });
        });

        it('edit form data', () => {
            cy.formField('name')
                .clear()
                .type(UPDATED_NAME)
                .submitButton()
                .click();
            cy.url().should('not.match', /\/domains\/\d+\/apis\/\d+/);
            cy.rows().should('have.length', 1);
        });
    });

    describe('create', () => {
        it('create domain', () => {
            cy.dataCy('create-item').click();
            cy.url().should('match', /\/domains\/\d+\/apis\/new/);
            cy.formField('name')
                .type('new')
                .formField('accessTokenExpiresIn')
                .type('15')
                .formField('identifier')
                .type('xxx')
                .submitButton()
                .click();
            cy.url().should('not.match', /\/domains\/\d+\/apis\/new/);
        });

        it('create app with same name should fail', () => {
            cy.dataCy('create-item').click();
            cy.url().should('match', /\/domains\/\d+\/apis\/new/);
            cy.formField('name')
                .type('new')
                .formField('accessTokenExpiresIn')
                .type('15')
                .formField('identifier')
                .type('xxx')
                .submitButton()
                .click();
            cy.url().should('match', /\/domains\/\d+\/apis\/new/);
            cy.cancelButton().click();
        });
    });

    describe('list', () => {
        it('rows count : sample + created', () => {
            cy.rows().should('have.length', 2);
        });

        it('latest item on top', () => {
            cy.rows(0).get('td').first().should('contain.text', 'new');
        });

        it.skip('sort by created change rows positions', () => {
            cy.get('table thead th').eq(1).click().click();
            cy.rows(0, 0).should('contain.text', UPDATED_NAME);
            cy.rows(1, 0).should('contain.text', 'new');
        });
    });

    describe('filter', () => {
        it('after reset filter there should be 1 rows', () => {
            cy.dataCy('text-search').type('new').type('{enter}');
            cy.rows().should('have.length', 1);
        });
    });

    describe.skip('delete', () => {
        it('remove', () => {
            cy.rows().find('.table-actions a').eq(1).click();
            cy.confirmYesButton().click();
            cy.rows().should('have.length', 1);
        });
    });
});
