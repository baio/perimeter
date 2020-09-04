import { EMAIL, PASSWORD, clearLocalStorage } from './_setup';

// tslint:disable: no-unused-expression
describe('apps', () => {
    const UPDATED_NAME = 'updated name';
    before(() => cy.reinitDb(true));
    before(() => {
        cy.visit('/domains/2/apps');
    });
    describe('edit', () => {
        it('load app edit form data', () => {
            cy.rows(0).click();
            cy.url().should('match', /\/domains\/\d+\/apps\/\d+/);
            cy.formField('name').should((input) => {
                const val = input.val();
                expect(val).be.not.empty;
            });
        });

        it('edit form data', () => {
            cy.formField('name')
                .clear()
                .type(UPDATED_NAME)
                .formField('idTokenExpiresIn')
                .clear()
                .type('100')
                .formField('refreshTokenExpiresIn')
                .clear()
                .type('100')
                .formField('allowedCallbackUrls')
                .clear()
                .type('http://test')
                .formField('allowedLogoutCallbackUrls')
                .clear()
                .type('http://test')
                .submitButton()
                .click();
            cy.url().should('not.match', /\/domains\/\d+\/apps\/\d+/);
            cy.rows().should('have.length', 1);
        });
    });

    describe('create', () => {
        it('create domain', () => {
            cy.dataCy('create-item').click();
            cy.url().should('match', /\/domains\/\d+\/apps\/new/);
            cy.formField('name').type('new').submitButton().click();
            cy.url().should('not.match', /\/domains\/\d+\/apps\/new/);
        });

        it('create app with same name should fail', () => {
            cy.dataCy('create-item').click();
            cy.url().should('match', /\/domains\/\d+\/apps\/new/);
            cy.formField('name').type('new').submitButton().click();
            cy.url().should('match', /\/domains\/\d+\/apps\/new/);
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

        it('sort by created change rows positions', () => {
            cy.get('table thead th').eq(4).click().click();
            cy.rows(0, 0).should('contain.text', UPDATED_NAME);
            cy.rows(1, 0).should('contain.text', 'new');
        });

        it('sort by name change rows positions', () => {
            cy.get('table thead th').eq(0).click();
            cy.rows(1, 0).should('contain.text', 'updated');
            // cy.rows(1, 0).should('contain.text', UPDATED_NAME);
            cy.get('table thead th').eq(0).click();
            // cy.rows(0, 0).should('contain.text', UPDATED_NAME);
            cy.rows(0, 0).should('contain.text', 'updated');
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
            cy.rows().find('.table-actions a').eq(0).click();
            cy.confirmYesButton().click();
            cy.rows().should('have.length', 0);
        });
    });
});
