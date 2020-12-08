using System;
using System.Collections.Generic;
using System.Text;

namespace AnanasCore.Wrapping
{
    /// <summary>
    /// Class to wrap an association between two classes
    /// </summary>
    public class AssociationWrapper
    {
        public ClassWrapper AssociationPartnerClass { get; private set; }
        public PropertyWrapper AssociationPartnerPrimaryKeyMember { get; private set; }
        public PropertyWrapper AssociationPartner { get; private set; }
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
