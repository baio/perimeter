import { HlcNzForm, HlcNzFormFields } from '@nz-holistic/nz-forms';
import { Observable, concat } from 'rxjs';
import { delay, filter, map, take } from 'rxjs/operators';
import { AdminForm, AdminFormFields } from '../models';
import { pipe, assocPath } from 'lodash/fp';
import { HlcFormFields } from '@nz-holistic/forms';
import { FormGroup } from '@angular/forms';

export const delayWhile = <T>(
    status$: Observable<T>,
    checker: () => boolean
): Observable<T> => {
    //
    const valueWithStatus1$ = status$.pipe(delay(0), filter(checker), take(1));
    // Regular status change
    const valueWithStatus2$ = status$.pipe(filter(checker));
    // Combine both
    return concat(valueWithStatus1$, valueWithStatus2$);
};

//

const mapTranslateFieldValue = (t: (s?: string) => string) => (
    field: HlcFormFields.FormFieldProp<string>
): HlcFormFields.FormFieldProp<string> =>
    field && (typeof field === 'string' ? t(field) : field.pipe(map(t)));

const addFieldTranslation = (t: (s?: string) => string) => (
    field: HlcNzFormFields.FormField
): HlcNzFormFields.FormField => {
    const tt = mapTranslateFieldValue(t);
    if (field.label) {
        field = { ...field, label: t(field.label) };
    }
    if (field.props?.label) {
        field = assocPath(['props', 'label'], tt(field.props?.label), field);
    }
    return field;
};

const addFieldsDefinitionTranslations = (t: (s: string) => string) => (
    definition: HlcNzForm.FieldsLayout<
        AdminFormFields.FormField,
        AdminFormFields.FieldWrapper
    >
): AdminForm.FormDefinition => {
    return {
        ...definition,
        fields: definition.fields.map(addFieldTranslation(t)),
    };
};

const addTabDefinitionTranslations = (t: (s: string) => string) => (
    definition: HlcNzForm.TabLayout<
        AdminFormFields.FormField,
        AdminFormFields.FieldWrapper
    >
): AdminForm.FormDefinition => {
    return {
        ...definition,
        title: mapTranslateFieldValue(t)(definition.title),
        $content: definition.$content.map((x) =>
            _addDefinitionTranslations(t, x)
        ) as any,
    };
};

const _addDefinitionTranslations = (
    t: (s: string) => string,
    definition: AdminForm.FormDefinition
): AdminForm.FormDefinition => {
    if (definition.kind === 'fields') {
        return addFieldsDefinitionTranslations(t)(definition);
    } else if (definition.kind === 'tabs') {
        return {
            ...definition,
            $content: definition.$content.map(
                addTabDefinitionTranslations(t) as any
            ),
        };
    } else {
        return definition;
    }
};

export const addDefinitionTranslations = (
    t: (s: string) => string,
    definition:
        | AdminForm.FormDefinition
        | ((form: FormGroup) => AdminForm.FormDefinition)
): AdminForm.FormDefinition | ((form: FormGroup) => AdminForm.FormDefinition) =>
    typeof definition === 'function'
        ? (form) => _addDefinitionTranslations(t, definition(form))
        : _addDefinitionTranslations(t, definition);
