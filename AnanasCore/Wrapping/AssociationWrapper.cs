using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.Wrapping
{
    public class AssociationWrapper//<T> where T : PersistentObject
    {
        public ClassWrapper AssociationPartnerClass { get; private set; }
        public FieldWrapper AssociationPartnerPrimaryKeyMember { get; private set; }
        public FieldWrapper AssociationPartner { get; private set; }
        public string AssociationName { get; private set; }
        public bool IsAnonymous { get { return AssociationPartner == null; } }
        public string ReferencingPrimaryKeyName { get { return AssociationPartnerPrimaryKeyMember?.Name; } }

        public AssociationWrapper(ClassWrapper foreignType, string associationName)
        {
            AssociationPartnerClass = foreignType;
            AssociationPartnerPrimaryKeyMember = foreignType.GetPrimaryKeyMember();

            // case there is a AssociationAnnotation
            if (associationName != null && !associationName.Equals(""))
            {
                AssociationName = associationName;
                AssociationPartner = AssociationPartnerClass.GetWrappedAssociation(associationName);
            }
        }
    }
}
