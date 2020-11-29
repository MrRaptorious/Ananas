using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.Wrapping
{
    public class AssociationWrapper//<T> where T : PersistentObject
    {
        private readonly ClassWrapper associationPartnerClass;
        private readonly FieldWrapper associationPartnerPrimaryKeyMember;
        private FieldWrapper associationPartner;
        private string associationName;

        public AssociationWrapper(ClassWrapper foreignType, string associationName)
        {
            this.associationPartnerClass = foreignType;
            associationPartnerPrimaryKeyMember = foreignType.GetPrimaryKeyMember();

            // case there is a AssociationAnnotation
            if (associationName != null && !associationName.Equals(""))
            {
                this.associationName = associationName;
                associationPartner = associationPartnerClass.getWrappedAssociation(associationName);
            }
        }

        public bool isAnonymous()
        {
            return associationPartner == null;
        }

        public FieldWrapper getAssociationPartner()
        {
            return associationPartner;
        }

        public string getAssociationName()
        {
            return associationName;
        }

        public ClassWrapper getReferencingType()
        {
            return associationPartnerClass;
        }



        public String getReferencingPrimaryKeyName()
        {
            return associationPartnerPrimaryKeyMember.name;
        }
    }
}
