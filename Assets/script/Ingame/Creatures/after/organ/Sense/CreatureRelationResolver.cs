using UnityEngine;

public class CreatureRelationResolver : MonoBehaviour
{
    public bool CanTargetAsPrey(Resource self, Resource target)
    {
        if (self == null || target == null || self == target)
            return false;

        if (target.resourceCategory == category.grass)
            return self.resourceCategory == category.herbivore;

        if (self.resourceCategory == category.predator ||
            self.resourceCategory == category.highpredator ||
            self.resourceCategory == category.dominant)
        {
            if (target.resourceCategory >= category.predator && target.speciesID == self.speciesID)
                return false;
            return target.resourceCategory >= category.herbivore;
        }

        return false;
    }
}
