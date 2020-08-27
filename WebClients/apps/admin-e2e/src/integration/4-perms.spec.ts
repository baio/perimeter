import { clearLocalStorage } from "./_setup";

// tslint:disable: no-unused-expression
describe('perms', () => {
    const UPDATED_NAME = 'write:all';
    before(() => cy.reinitDb(true));
    before(() => {
        cy.visit('/domains/2/apps');
        cy.dataCy('apis-menu-item').click();
        cy.rows(0, 1).click();
    });

    it('app should be open', () => {
        cy.url().should('include', 'permissions');
    });

    describe('create', () => {
        it('create read permission', () => {
            cy.dataCy('create-item').click();
            cy.url().should(
                'match',
                /\/domains\/\d+\/apis\/\d+\/permissions\/new/
            );
            cy.formField('name')
                .type('read:all')
                .formField('description')
                .type('User will be able to read all items')
                .submitButton()
                .click();
            cy.url().should(
                'not.match',
                /\/domains\/\d+\/apis\/\d+\/permissions\/new/
            );
        });

        it('create permission with same name should fail', () => {
            cy.dataCy('create-item').click();
            cy.url().should(
                'match',
                /\/domains\/\d+\/apis\/\d+\/permissions\/new/
            );
            cy.formField('name')
                .type('read:all')
                .formField('description')
                .type('User will be able to read all items')
                .submitButton()
                .click();
            cy.url().should(
                'match',
                /\/domains\/\d+\/apis\/\d+\/permissions\/new/
            );
            cy.cancelButton().click();
        });

        it('create write permission', () => {
            cy.dataCy('create-item').click();
            cy.url().should(
                'match',
                /\/domains\/\d+\/apis\/\d+\/permissions\/new/
            );
            cy.formField('name')
                .type('write')
                .formField('description')
                .type('User will be able to write all items')
                .submitButton()
                .click();
            cy.url().should(
                'not.match',
                /\/domains\/\d+\/apis\/\d+\/permissions\/new/
            );
        });
    });

    describe('edit', () => {
        it('load app edit form data', () => {
            cy.rows(0).click();
            cy.url().should(
                'match',
                /\/domains\/\d+\/apis\/\d+\/permissions\/\d+/
            );
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
            cy.url().should(
                'not.match',
                /\/domains\/\d+\/apis\/\d+\/permissions\/\d+/
            );
            cy.rows().should('have.length', 2);
        });
    });

    describe('list', () => {
        it('rows count : sample + created', () => {
            cy.rows().should('have.length', 2);
        });

        it('latest item on top', () => {
            cy.rows(0, 0).should('contain.text', 'write:all');
            cy.rows(1, 0).should('contain.text', 'read:all');
        });

        it('sort by created change rows positions', () => {
            cy.get('table thead th').eq(2).click().click();
            cy.rows(0, 0).should('contain.text', 'read:all');
            cy.rows(1, 0).should('contain.text', 'write:all');
        });
    });

    describe('filter', () => {
        it('after reset filter there should be 1 rows', () => {
            cy.dataCy('text-search').type('read').type('{enter}');
            cy.rows().should('have.length', 1);
        });
    });

    // wtf ?
    describe.skip('delete', () => {
        it('remove', () => {
            cy.rows(0, 3)
                .find('.table-actions a')
                .eq(0)
                .click()
                .confirmYesButton()
                .click()
                .rows()
                .should('have.length', 0);
        });
    });
});
